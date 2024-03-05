#include <avr/eeprom.h>
#include <string.h>
#include <avr/wdt.h>
#include "ButtonUtility.h"
#include "BootloadUtility.h"
#include "Timer.h"

const uint8_t ack[] = {'\r'};

int main(void)
{
    // Clear watchdog reset flag and disable watchdog timer.
    MCUCSR &= ~(1 << WDRF);
    wdt_disable();
    
    uint8_t dataBuffer[259];
    uint16_t bufferCounter = 0;
    uint8_t communicationTimeout = 20;
    bool writingToFlash = false;
    bool applicationExist = eeprom_read_byte(bootloaderStatusAddress) == uploadCompleteCode;

    enableTimer();

    // Return to application section.
    if (applicationExist && !pressAndHold(&PINE, PE4, 5))
    {
        disableTimer();
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    enableUSART();
    startBootloadIndicator();

    while (true)
    {
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

        // Check for data in the usart receive buffers.
        struct DataElement dataStruct = usartReceive();

        if (!dataStruct.dataReceived)
            continue;

        dataBuffer[bufferCounter] = dataStruct.data;
        ++bufferCounter;

        // Check the data buffer for the "RTU\0" command.
        if (dataStruct.data == '\0' && !writingToFlash)
        {
            bufferCounter = 0;

            if (!memcmp(dataBuffer, "RTU", 4))
            {
                communicationTimeout = 5;
                writingToFlash = true;
                startBootloadProcess();
            }

            continue;
        }

        // The microcontroller is writing to flash. Send a byte acknowledgement back to the server.
        if (writingToFlash)
        {
            usartTransmit(ack, 1);
        }

        // Check if the data buffer has been completely filled with page data
        if (bufferCounter == 259)
        {
            bufferCounter = 0;
            communicationTimeout = 5;
            writeProgramDataToFlash(dataBuffer);
        }
    }
}