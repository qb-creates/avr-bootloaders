#include "ButtonPressUtility.h"
#include "CommandUtility.h"
#include "Timer.h"
#include <avr/eeprom.h>
#include <avr/wdt.h>

int main(void)
{
    bool applicationExist = eeprom_read_byte(bootloaderStatusAddress) == uploadCompleteCode;
    uint8_t dataBuffer[259];
    uint8_t resetTimer = 0;
    bool writeToFlash = false;
    const uint16_t bufferMaxSize = 259;
    uint16_t bufferCounter = 0;

    enableTimer();

    // Return to application section
    if (applicationExist && !pressAndHold(&PINE, PE4, 5))
    {
        disableTimer();
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    enableUSART();
    startBootloaderIndicator(5);

    while (true)
    {
        struct DataStruct dataStruct = usartReceive(dataBuffer);

        if (dataStruct.dataReceived)
        {
            dataBuffer[bufferCounter] = dataStruct.data;
            ++bufferCounter;

            if (dataStruct.data == '\0' && !writeToFlash)
            {
                bufferCounter = 0;
                resetTimer = 0;
                writeToFlash = true;
                executeCommand(dataBuffer);
            }
            else if (bufferCounter == bufferMaxSize)
            {
                bufferCounter = 0;
                resetTimer = 0;
                checkForPage(dataBuffer);
            }
        }

        if (ETIFR & _BV(OCF3A) && (applicationExist || writeToFlash))
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