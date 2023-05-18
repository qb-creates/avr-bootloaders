#include <avr/io.h>
#include <Timer.h>

/**
 * @brief Configure 8-bit Timer0. It is set to Compare Output Mode.
 * Output OC1A has no operation. A 1024 prescale factor is selected.
 * Enables compare match interrupt.
 * */
void TIMER0::configure()
{
    TCCR0 = 1 << WGM01 | 1 << CS02 | 1 << CS01 | 1 << CS00;
    OCR0 = 72;

    // Enable compare value interrupt for Timer1A and Timer0
    TIMSK |= _BV(OCIE0);
}

void TIMER0::adjustSpeed(int ocr0)
{
    TCNT0 = 0;
    OCR0 = ocr0;
}

/**
 * @brief Configure 16-bit Timer1A. It is set to Compare Output Mode.
 * Output OC1A has no operation. A 1024 prescale factor is selected.
 * Enables compare match interrupt.
 * */
void TIMER1A::configure()
{
    TCCR1B = _BV(WGM12) | _BV(CS12) | _BV(CS10);
    OCR1A = 720;
    TIMSK |= _BV(OCIE1A);
}

/**
 * @brief Adjust the speed of Timer1A
 *
 * @param[in] ocr1a Value betwen 0 and 65535. This will change how often the compare
 * match interupt is triggered. Which will change the timer delay.
 */
void TIMER1A::adjustSpeed(int ocr1a)
{
    TCNT1 = 0;
    OCR1A = ocr1a;
}

/**
 * @brief Configure 16-bit Timer1A. It is set to Compare Output Mode.
 * Output OC1A has no operation. A 1024 prescale factor is selected.
 * Enables compare match interrupt.
 * */
void TIMER3A::configure()
{
    TCCR3B = _BV(WGM32) | _BV(CS32) | _BV(CS30);
    OCR3A = 120;
    ETIMSK |= _BV(OCIE3A);
}

/**
 * @brief Adjust the speed of Timer1A
 *
 * @param[in] ocr1a Value betwen 0 and 65535. This will change how often the compare
 * match interupt is triggered. Which will change the timer delay.
 */
void TIMER3A::adjustSpeed(int ocr3a)
{
    TCNT3 = 0;
    OCR3A = ocr3a;
}