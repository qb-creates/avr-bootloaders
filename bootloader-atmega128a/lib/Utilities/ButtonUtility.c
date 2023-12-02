#include <avr/io.h>
#include "ButtonUtility.h"
#include "Timer.h"

/**
 * @brief Press and hold function for buttons. 
 * 
 * @param inputPins Input pins address that the button is connected to
 * @param pin The pin number the button is connected to.
 * @param holdTime The ammount of time the button must be held in seconds.
 * @return Returns true if the button was held for the specified ammount of time. Returns false if the button was released before the specified amount of time has passed.
 * 
 * @attention - The data register is configured to enable the pull up resistor for the specified pin. Button must be shorted to ground 
 * to trigger the input.
 * @attention - Bootloader timer is configured to trigger the output compare flag every 1 second.
 * 
 */
bool pressAndHold(volatile uint8_t *inputPins, uint8_t pin, uint8_t holdTime)
{
    volatile uint8_t *dataDirectionRegister = inputPins + 1;
    volatile uint8_t *dataRegister = inputPins + 2;

    // Set the data direction register to be all inputs.
    *dataDirectionRegister = 0x00;

    // Enable pull up resister for the specified pin.
    *dataRegister = _BV(pin);

    while (!(*inputPins & _BV(pin)) && (holdTime != 0))
    {
        if (checkOutputCompareFlag())
        {
            clearTimerFlag();
            --holdTime;
        }
    }

    *dataRegister = 0x00;
    return (holdTime == 0);
}