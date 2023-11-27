#include <avr/io.h>
#include "ButtonPressUtility.h"

/**
 * @brief 
 * 
 * @param inputPins Input pins address that the button is connected to
 * @param pin The pin the button is connected to.
 * @param holdTime The ammount of time the button must be held in seconds.
 * @return Returns true if the button was held for the specified ammount of time. Returns false if the button was released before the specified amount of time has passed.
 */
bool pressAndHold(volatile uint8_t *inputPins, uint8_t pin, uint8_t holdTime)
{
    volatile uint8_t *dataDirectionRegister = inputPins + 1;
    volatile uint8_t *dataRegister = inputPins + 2;
    uint8_t timer = 0;

    // Set the data direction register to be all inputs. Increment the Data Direction Register address to get the Data Register address
    *dataDirectionRegister = 0x00;

    // Enable pull up resister for the specified pin.
    *dataRegister = _BV(pin);

    while (!(*inputPins & _BV(pin)) && (timer != holdTime))
    {
        if (ETIFR & _BV(OCF3A))
        {
            ETIFR |= _BV(OCF3A);
            ++timer;
        }
    }

    *dataRegister = 0x00;
    return (timer == holdTime);
}