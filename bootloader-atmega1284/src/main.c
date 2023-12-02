#include <avr/eeprom.h>
#include <string.h>
#include <avr/wdt.h>
#include "ButtonUtility.h"
#include "BootloadUtility.h"

int main(void)
{
    // Clear watchdog reset flag and disable watchdog timer.
    MCUSR &= ~(1 << WDRF);
    wdt_disable();

    uint8_t dataBuffer[259];
    uint16_t bufferCounter = 0;
    bool writeToFlash = false;
    bool applicationExist = eeprom_read_byte(bootloaderStatusAddress) == uploadCompleteCode;

    // Return to application section.
    if (applicationExist && !pressAndHold(&PIND, PD2, 5000))
    {
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    enableUSART();
    startBootloadIndicator();

    if (applicationExist)
    {
        wdt_enable(WDTO_8S);
    }
    
    while (true)
    {
        // Check for data in the usart receive buffers.
        struct DataElement dataStruct = usartReceive();

        if (!dataStruct.dataReceived)
            continue;

        wdt_reset();
        dataBuffer[bufferCounter] = dataStruct.data;
        ++bufferCounter;

        // Check the data buffer for the "RTU\0" command.
        if (dataStruct.data == '\0' && !writeToFlash)
        {
            bufferCounter = 0;
            
            if (!memcmp(dataBuffer, "RTU", 4))                
            {
                writeToFlash = true;
                wdt_enable(WDTO_4S);
                startBootloadProcess();
            }

            continue;
        }
        
        // Check if the data buffer has been completely filled with page data
        if (bufferCounter == 259)
        {
            bufferCounter = 0;
            writePageDataToFlash(dataBuffer);
        }
    }
}