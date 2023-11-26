#include <avr/io.h>
#include "ButtonPressUtility.h"

/**
 * @brief 
 * 
 * @param ddr The data direction register that the button is connected to
 * @param pin The pin the button is connected to.
 * @param holdTime The ammount of time the button must be held in seconds.
 * @return Returns true if the button was held for the specified ammount of time. Returns false if the button was released before the specified amount of time has passed.
 */
bool pressAndHold(volatile uint8_t *ddr, uint8_t pin, uint8_t holdTime)
{
    // Set the data direction register to be all inputs. Increment the Data Direction Register address to get the Data Register address
    *ddr++ = 0x00;
    
    // Enable pull up resister for the pin specified
    *ddr = _BV(pin);

    uint8_t timer = 0;
    
    while (!(PINE & _BV(PE4)) && (timer != holdTime))
    {
        if (ETIFR & _BV(OCF3A))
        {
            ETIFR |= _BV(OCF3A);
            ++timer;
        }
    }

    *ddr = 0;
    return (timer == holdTime);
}