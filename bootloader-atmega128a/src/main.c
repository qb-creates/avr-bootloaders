#include <avr/eeprom.h>
#include <string.h>
#include <avr/wdt.h>
#include "ButtonUtility.h"
#include "BootloadUtility.h"
#include "Timer.h"

int main(void)
{
    uint8_t dataBuffer[259];
    uint16_t bufferCounter = 0;
    uint8_t communicationTimeout = 20;
    bool writingToFlash = false;
    bool applicationExist = eeprom_read_byte(bootloaderStatusAddress) == uploadCompleteCode;

    enableTimer();

    // Return to application section
    if (applicationExist && !pressAndHold(&PINE, PE4, 5))
    {
        disableTimer();
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    enableUSART();
    startBootloaderIndicator();

    while (true)
    {
        struct DataElement dataStruct = usartReceive();

        if (dataStruct.dataReceived)
        {
            dataBuffer[bufferCounter] = dataStruct.data;
            ++bufferCounter;

            if (dataStruct.data == '\0' && !writingToFlash)
            {
                bufferCounter = 0;

                if (!memcmp(dataBuffer, "RTU", 4))
                {
                    startBootloadProcess();
                    communicationTimeout = 5;
                    writingToFlash = true;
                }
            }
            else if (bufferCounter == 259)
            {
                bufferCounter = 0;
                communicationTimeout = 5;
                writeProgramDataToFlash(dataBuffer);
            }
        }

        // Check Timer Output Compare Flag.        
        if (checkOutputCompareFlag() && (applicationExist || writingToFlash))
        {
            --communicationTimeout;
            clearTimerFlag();
        }

        // Reset Microcontroller.
        if (communicationTimeout == 0)
        {
            wdt_enable(WDTO_1S);
            while (true);
        }
    }
}