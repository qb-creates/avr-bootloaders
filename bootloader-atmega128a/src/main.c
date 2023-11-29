#include <avr/eeprom.h>
#include <avr/wdt.h>
#include "ButtonUtility.h"
#include "CommandUtility.h"
#include "Timer.h"

int main(void)
{
    uint8_t dataBuffer[259];
    uint16_t bufferCounter = 0;
    uint8_t resetTimer = 0;
    bool writeToFlash = false;
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
                writeToFlash = true;
                executeCommand(dataBuffer);
            }
            else if (bufferCounter == 259)
            {
                bufferCounter = 0;
                checkForPage(dataBuffer);
            }
            
            resetTimer = 0;
        }

        if (checkOutputCompareFlag() && (applicationExist || writeToFlash))
        {
            clearTimerFlag();
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