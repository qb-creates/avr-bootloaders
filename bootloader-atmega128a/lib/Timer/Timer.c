#include <avr/io.h>
#include "Timer.h"

/**
 * @brief Configure 16-bit Timer3. 
 * 
 * @note - Output compare mode.
 * @note - Output OC3A/B/C no operation.
 * @note - A 1024 prescale.
 * @note - Output compare match will happen every 1 second.
 * @note - OCR3A = (F_CPU / (2 * 1024 * frequency)) - 1;
 * 
 * @attention A clock frequency of 7.3728MHz was used during the development of this bootloader.
 * Update F_CPU and the value of the 'frequency' variable to get the desired output compare match time.
 * 
 */
void enableTimer(void)
{
    TCCR3B = _BV(WGM32) | _BV(CS32) | _BV(CS30);
    OCR3A = (F_CPU / (2 * 1024 * .5)) - 1;
}
