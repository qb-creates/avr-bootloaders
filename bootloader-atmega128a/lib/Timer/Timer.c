#include <avr/io.h>
#include "Timer.h"

/**
 * @brief Configure 16-bit Timer3. 
 * 
 * @param frequency The frequency at which the connected indicator light will flash.
 * @note The timer is configured to be in output compare mode.
 * @note Output OC3A/B/C have no operation.
 * @note A 1024 prescale factor is selected.
 * @note Output compare match will happen every 1 second.
 * @note OCR3A = (F_CPU / (2 * 1024 * frequency)) - 1;
 */
void enableTimer(void)
{
    TCCR3B = _BV(WGM32) | _BV(CS32) | _BV(CS30);
    OCR3A = (F_CPU / (2 * 1024 * .5)) - 1;
}
