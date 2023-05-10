#include <avr/io.h>
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <avr/wdt.h>
#include <stdbool.h>
#include "USART.h"

void boot_program_page(volatile uint8_t *buf);

const char *update = "update\r";

volatile uint8_t message[258];
volatile uint16_t messageCounter = 0;
volatile bool updateComplete = false;
volatile bool updatingFlash = false;
volatile bool writePage = false;
volatile int timer = 0;

ISR(TIMER1_COMPA_vect)
{
  if (updatingFlash)
  {
    timer++;

    if (timer == 2)
    {
      writePage = true;
    }

    if (timer == 5)
    {
      updatingFlash = false;
      updateComplete = true;
    }
  }
}

ISR(USART1_RX_vect)
{
  char data = USART1::receiveData();

  if (!updatingFlash)
  {
    message[messageCounter] = data;
    messageCounter++;
    if (data == '\r')
    {
      messageCounter = 0;
      for (int i = 0; i < 7; i++)
      {
        if (message[i] != update[i])
        {
          printf("NACK\n");
          return;
        }
      }
      updatingFlash = true;
      printf("Entering Firmware Update Mode\n");
    }
  }
  else
  {
    timer = 0;
    message[messageCounter] = data;
    messageCounter++;
  }
}
void ConfigureTimer(void)
{
  TCCR1B = 1 << WGM12 | 1 << CS12 | 0 << CS11 | 1 << CS10;
  OCR1A = 7200;
  TIMSK = 1 << OCIE1A;
  TCNT1 = 0;
}
int main(void)
{
  DDRB = 0x00;
  PORTB = 0xFF;
  _delay_ms(1000);

  if ((PINB & 0x01) && pgm_read_word(0) != 0xFFFF)
  {
    cli();
    asm("jmp 0x000");
  }
  else
  {
    DDRC = 0xFF;
    PORTC = 0xEF;
    MCUCR = _BV(IVCE);
    MCUCR = _BV(IVSEL);
    USART1::configure(47, Databits::Eight, Parity::None, StopBits::Two, ClkPolarity::FallingRising, Mode::Async);
    ConfigureTimer();
    sei();
  }

  while (1)
  {
    if (writePage && updatingFlash)
    {
      boot_program_page(message);
      printf("Page Written\n");
    }

    if (updateComplete)
    {
      printf("Update Complete\n");
      updateComplete = false;
      wdt_enable(WDTO_30MS);
    }

    if (!(PINB & 0x02))
    {
      // Use this to see all bytes of data for a particular section
      for (int i = 0; i < SPM_PAGESIZE * 20; i++)
      {
        printf("%02x  ", pgm_read_byte(i));
      }
    }
  }
}

void boot_program_page(volatile uint8_t *buf)
{
  uint16_t page = (*buf++) << 8;
  page = (page + *buf++) * 256;

  // Uncomment to see what page is being written to
  // printf("page %02x written\n", page);

  cli();
  eeprom_busy_wait();
  boot_page_erase(page);
  boot_spm_busy_wait(); // Wait until the memory is erased.

  for (uint16_t i = 0; i < (messageCounter)-2u; i += 2)
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
  messageCounter = 0;
}
