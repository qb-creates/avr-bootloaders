#include "Bootloader.h"
#include "Timer.h"
#include "USART.h"
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <string.h>
#include <stdbool.h>
void executeCommand(void);

const char *requestSignatureBytes = "RSB";
const char *requestBootConfig = "RBC";
const char *requestToUpdate = "RTU";
const uint8_t signatureBytes[] = {0x1E, 0x97, 0x02};
const uint8_t clearToUpdateAck[] = {'C', 'T', 'U'};
const uint8_t pageAck[] = {'P', 'a', 'g', 'e'};

int main(void)
{
    enableTimer();
    enableUSART();

    bool applicationExist = pgm_read_word(0) != 0xFFFF;
    uint8_t resetTimer = 0;

    if (!pressAndHold(5) && applicationExist)
    {
        disableTimer();
        disableUSART();
        asm("jmp 0x000");
    }

    startBootloaderIndicator();

    while (true)
    {
        usartReceive();

        if (ETIFR & _BV(OCF3A))
        {
            ETIFR |= _BV(OCF3A);
            resetTimer++;
        }

        if (commandReceived)
        {
            commandReceived = false;
            executeCommand();
        }

        if (pageReceived)
        {
            pageReceived = false;
            uint8_t finalByte = writeProgramDataToFlash(programDataBuffer);
            usartTransmit(pageAck, 4);
        
            if (finalByte == 0xFE)
                break;
        }

        if (!writingToFlash && resetTimer >= 20 && applicationExist)
            break;
    }

    disableTimer();
    disableUSART();
    stopBootloaderIndicator();
    wdt_enable(WDTO_15MS);
}

void executeCommand(void)
{
    if (memcmp(commandDataBuffer, requestBootConfig, commandBufferMaxSize) == 0)
    {
        uint8_t highFuseBits[] = {boot_lock_fuse_bits_get(GET_HIGH_FUSE_BITS)};
        usartTransmit(highFuseBits, 1);
    }
    else if (memcmp(commandDataBuffer, requestSignatureBytes, commandBufferMaxSize) == 0)
    {
        usartTransmit(signatureBytes, 3);
    }
    else if (memcmp(commandDataBuffer, requestToUpdate, commandBufferMaxSize) == 0)
    {
        writingToFlash = true;
        usartTransmit(clearToUpdateAck, 3);
    }
}