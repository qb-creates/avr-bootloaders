#include <Timer.h>
#include <avr/interrupt.h>
#include <avr/io.h>

/**
 * @brief Configure 16-bit Timer3. It is set to Compare Output Mode.
 * Output OC3A/B/C have no operation. A 1024 prescale factor is selected.
 * Enables compare match interrupt.
 * */
void enableTimer()
{
    TCCR3B = _BV(WGM32) | _BV(CS32) | 1 << CS30;
    OCR3A = (F_CPU / (2 * 1024 * .5)) - 1;
}

/**
 * @brief Configure 16-bit Timer1A. It is set to Compare Output Mode.
 * Output OC1A is toggled on compare match every 100ms. PB5 can be
 * connected to an LED to let the user know when the device is in
 * bootloader mode.
 */
void disableTimer()
{
    TCCR3B = 0;
}