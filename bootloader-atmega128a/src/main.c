#include <avr/eeprom.h>
#include <avr/wdt.h>
#include "CommandUtility.h"
#include "Timer.h"

int main(void)
{
    enableTimer();
    enableUSART();

    uint8_t resetTimer = 0;
    uint8_t bootloaderByte = eeprom_read_byte((uint8_t *)46);
    uint8_t dataBuffer[259];

    // Return to application section
    if (bootloaderByte == 'c' && !pressAndHold(5))
    {
        disableTimer();
        disableUSART();
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    startBootloaderIndicator();

    while (true)
    {
        bool dataReceived = usartReceive(dataBuffer);
        checkForValidCommand(dataBuffer);
        bool uploadComplete = checkForPage(dataBuffer);

        if (dataReceived)
            resetTimer = 0;

        if (ETIFR & _BV(OCF3A) && bootloaderByte == 'c')
        {
            ETIFR |= _BV(OCF3A);
            ++resetTimer;
        }

        // Exit bootloader.
        if (resetTimer >= 20)
            break;

        if (uploadComplete)
        {
            eeprom_update_byte((uint8_t *)46, 'c');
            break;
        }
    }

    wdt_enable(WDTO_2S);

    while (true) {};
}