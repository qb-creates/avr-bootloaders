#include <avr/io.h>
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <util/delay.h>
#include <stdbool.h>
#include "USART.h"
#include "Timer.h"

void writePageToFlash(volatile uint8_t *buf);
int isCommandValid(volatile uint8_t data[]);

const char *updateCommand = "RTU\r";

volatile uint8_t usartData[259];
volatile uint16_t dataCounter = 0;

volatile bool waitingForCommand = true;
volatile bool commandReceived = false;
volatile bool verifyFlash = false;

volatile bool writingToFlash = false;

ISR(USART1_RX_vect)
{
  usartData[dataCounter] = USART1::receiveData();

  if (waitingForCommand)
  {
    if (usartData[dataCounter] == '\r')
    {
      commandReceived = true;
      dataCounter = 0;
      waitingForCommand = false;
      return;
    }
  }

  if (writingToFlash)
  {
    USART1::transmitData("\r");
  }

  dataCounter++;
}

int main(void)
{
  bool applicationExist = pgm_read_word(0) != 0xFFFF;
  int watchdogResetFlag = MCUCSR & _BV(WDRF);
  int powerOnResetFlag = MCUCSR & _BV(PORF);

  if ((watchdogResetFlag || powerOnResetFlag) && applicationExist)
  {
    MCUCSR &= ~(watchdogResetFlag | powerOnResetFlag);
    cli();
    asm("jmp 0x000");
  }

  MCUCR = _BV(IVCE);
  MCUCR = _BV(IVSEL);

  USART1::configure(3, Databits::Eight, Parity::None, StopBits::One, ClkPolarity::FallingRising, Mode::Async);
  TIMER0::configure();
  TIMER::startBootloaderIndicator();
  sei();

  while (1)
  {
    if (commandReceived)
    {
      switch (isCommandValid(usartData))
      {
      case 0:
        USART1::transmitData("NACK");
        waitingForCommand = true;
        break;
      case 1:
        writingToFlash = true;
        USART1::transmitData("CTU");
        break;
      }

      commandReceived = false;
    }

    if (dataCounter == 259 && writingToFlash)
    {
      char finalByte = usartData[dataCounter - 1];
      writePageToFlash(usartData);
      _delay_ms(50);

      if (finalByte != '\xFE')
      {
        dataCounter = 0;
        USART1::transmitData("Page");
      }
      else
      {
        TIMER::stopBootloaderTimer();
        TIMER::stopBootloaderIndicator();
        USART1::transmitData("Complete");
        wdt_enable(WDTO_15MS);
      }
    }

    if (!writingToFlash && TIMER::resetTimer >= 20 && applicationExist)
    {
      TIMER::stopBootloaderTimer();
      TIMER::stopBootloaderIndicator();
      wdt_enable(WDTO_15MS);
    }
  }
}

int isCommandValid(volatile uint8_t command[])
{
  for (int i = 0; i < 4; i++)
  {
    if (command[i] != updateCommand[i])
    {
      return 0;
    }
  }

  return 1;
}

void writePageToFlash(volatile uint8_t *buf)
{
  uint16_t page = (*buf++) << 8;
  page = (page + *buf++) * 256;

  cli();

  // Wait for any active eeprom writes to complete.
  eeprom_busy_wait();

  // Erase memory page.
  boot_page_erase(page);

  // Wait until the memory is erased.
  boot_spm_busy_wait();

  for (uint16_t i = 0; i < (dataCounter - 3); i += 2)
  {
    // Build opcode while setting up little-endian word.
    uint16_t opcode = *buf++;
    opcode += (*buf++) << 8;

    // Load page temp buffers
    boot_page_fill(page + i, opcode);
  }

  // Store buffer in flash page.
  boot_page_write(page);

  // Wait until the memory is written.
  boot_spm_busy_wait();

  // Enable Read While Write Section
  boot_rww_enable();
  sei();
}
