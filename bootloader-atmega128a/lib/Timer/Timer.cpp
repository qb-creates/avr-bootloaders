#include <Timer.h>
volatile int TIMER::resetTimer = 0;

ISR(TIMER3_COMPA_vect)
{
  TIMER::resetTimer++;
}

/**
 * @brief Configure 16-bit Timer3. It is set to Compare Output Mode.
 * Output OC3A/B/C have no operation. A 1024 prescale factor is selected.
 * Enables compare match interrupt.
 * */
void TIMER0::configure()
{
  TCCR3B = _BV(WGM32) | _BV(CS32) | 1 << CS30;
  OCR3A = (F_CPU / 2048) * 2;

  // Enable compare value interrupt for Timer3 channel A.
  ETIMSK = _BV(OCIE3A);
}

/**
 * @brief Configure 16-bit Timer1A. It is set to Compare Output Mode.
 * Output OC1A is toggled on compare match every 100ms. PB5 can be
 * connected to an LED to let the user know when the device is in
 * bootloader mode.
 */
void TIMER::stopBootloaderTimer()
{
  TCCR3B = 0;
  TIMER::resetTimer = 0;
}

/**
 * @brief Configure 16-bit Timer1. It is set to Compare Output Mode.
 * Output OC1A is toggled on compare match every 100ms. PB5 can be
 * connected to an LED to let the user know when the device is in
 * bootloader mode.
 */
void TIMER::startBootloaderIndicator()
{
  DDRB |= _BV(PB5);
  TCCR1B = _BV(WGM12) | _BV(CS12) | _BV(CS10);
  TCCR1A = _BV(COM1A0);
  OCR1A = (F_CPU / 2048) * .2;
}

/**
 * @brief Stops the bootloader indicator output.
 */
void TIMER::stopBootloaderIndicator()
{
  TCCR1B = 0x00;
  TCCR1A = 0x00;
  PORTB |= _BV(PB5);
}