#include <avr/eeprom.h>
#include <avr/wdt.h>
#include "CommandUtility.h"
#include "ButtonPressUtility.h"
#include "Timer.h"

int main(void)
{
    bool applicationExist = eeprom_read_byte(bootloaderStatusAddress) == uploadCompleteCode;
    uint8_t dataBuffer[259];
    uint8_t resetTimer = 0;

    enableTimer();

    // Return to application section
    if (applicationExist && !pressAndHold(&DDRE, PE4, 5))
    {
        disableTimer();
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    enableUSART();
    startBootloaderIndicator(5);

    while (true)
    {
        enum DataString dataStringType = usartReceive(dataBuffer);
        
        if (dataStringType == CommandString)
        {
            resetTimer = 0;
            executeCommand(dataBuffer);
        }
        else if (dataStringType == PageString)
        {
            resetTimer = 0;
            checkForPage(dataBuffer);
        }

        if (ETIFR & _BV(OCF3A) && applicationExist)
        {
            ETIFR |= _BV(OCF3A);
            ++resetTimer;
        }

        // Reset Microcontroller.
        if (resetTimer >= 20)
        {
            stopBootloaderIndicator();
            wdt_enable(WDTO_2S);
            while (true);
        }
    }
}