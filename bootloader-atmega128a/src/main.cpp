#include <avr/io.h>
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <avr/boot.h>
#include <util/delay.h>
#include <stdbool.h>
#include "USART.h"
#include "Timer.h"

void configureTimer(void);
void writePageToFlash(volatile uint8_t *buf);

const char *update = "RTU\r";

volatile uint8_t pageData[258];
volatile uint16_t dataCounter = 0;

volatile bool writingToFlash = false;
volatile bool writePage = false;
volatile bool updateComplete = false;
volatile bool resetMCU = false;
volatile int timer = 0;
volatile int resetTiemr = 0;
volatile bool applicationExist = false;
volatile bool verifyFlash = false;
ISR(TIMER0_COMP_vect)
{
  resetTiemr++;

  if (writingToFlash)
  {
    timer++;
    writePage = timer == 20;
    writingToFlash = !(timer == 100);
    updateComplete = timer == 100;
  }

  if (!writingToFlash && applicationExist)
  {
    resetMCU = resetTiemr == 2000;
  }
}

ISR(TIMER1_COMPA_vect)
{
  PORTC ^= 0x01;
}

ISR(USART1_RX_vect)
{
  char data = USART1::receiveData();

  if (!writingToFlash)
  {
    pageData[dataCounter] = data;
    dataCounter++;

    if (data == '\r')
    {
      dataCounter = 0;
      for (int i = 0; i < 4; i++)
      {
        if (pageData[i] != update[i])
        {
          printf("NACK");
          return;
        }
      }
      writingToFlash = true;
      TCNT0 = 0;
      printf("CTU");
    }
  }
  else
  {
    TCNT1 = 0;
    timer = 0;
    pageData[dataCounter] = data;
    dataCounter++;
    printf("\r");
  }
}

int main(void)
{
  // uint8_t sr[3];
  // sr[0] = boot_signature_byte_get(0x0000);
  // sr[1] = boot_signature_byte_get(0x0002);
  // sr[2] = boot_signature_byte_get(0x0004);

  bool watchdogResetFlag = MCUCSR & _BV(WDRF);
  applicationExist = pgm_read_word(0) != 0xFFFF;

  if (watchdogResetFlag && applicationExist)
  {
    MCUCSR &= ~_BV(WDRF);
    cli();
    asm("jmp 0x000");
  }

  DDRC = 0xFF;
  PORTC = 0xFE;
  // DDRB = 0x00;
  // PORTB = 0xFF;
  MCUCR = _BV(IVCE);
  MCUCR = _BV(IVSEL);

  USART1::configure(3, Databits::Eight, Parity::None, StopBits::One, ClkPolarity::FallingRising, Mode::Async);
  TIMER0::configure();
  TIMER1A::configure();
  sei();

  while (1)
  {
    // if (!(PINB & 0x02))
    // {
    //   // Use this to see all bytes of data for a particular section
    //   for (uint64_t i = 0; i < (SPM_PAGESIZE * 300); i++)
    //   {
    //     printf("%02x  ", pgm_read_byte(i));
    //   }
    //   wdt_enable(WDTO_15MS);
    // }

    if (resetMCU)
    {
      resetMCU = false;
      wdt_enable(WDTO_15MS);
    }

    if (writePage && writingToFlash)
    {
      writePageToFlash(pageData);
      printf("Page");
    }

    if (updateComplete)
    {
      printf("Complete");
      updateComplete = false;
      wdt_enable(WDTO_15MS);
    }
  }
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

  for (uint16_t i = 0; i < (dataCounter - 2); i += 2)
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

  writePage = false;
  dataCounter = 0;
}
