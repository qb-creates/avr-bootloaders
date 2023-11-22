#include "Bootloader.h"
#include "Timer.h"
#include "USART.h"
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <string.h>

void executeCommand(void);

const char *requestSignatureBytes = "RSB";
const char *requestBootConfig = "RBC";
const char *requestToUpdate = "RTU";
const char signatureBytes[] = {0x00, 0x00, 0x00};

int main(void)
{
    enableTimer();
    configureUSART();

    bool applicationExist = pgm_read_word(0) != 0xFFFF;
    int resetTimer = 0;

    if (!pressAndHold(5) && applicationExist)
    {
        disableTimer();
        asm("jmp 0x000");
    }

    startBootloaderIndicator();

    while (true)
    {
        receiveData();

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
            char finalByte = writeProgramDataToFlash(programDataBuffer);
            transmitString("Page");

            if (finalByte == '\xFE')
                break;
        }

        if (!writingToFlash && resetTimer >= 20 && applicationExist)
            break;
    }

    disableTimer();
    stopBootloaderIndicator();
    wdt_enable(WDTO_15MS);
}

void executeCommand(void)
{
    if (memcmp(commandDataBuffer, requestBootConfig, commandBufferMaxSize) == 0)
    {
        unsigned char highFuseBits = boot_lock_fuse_bits_get(GET_HIGH_FUSE_BITS);
        transmitChar(highFuseBits);
    }
    else if (memcmp(commandDataBuffer, requestSignatureBytes, commandBufferMaxSize) == 0)
    {
        transmitString(signatureBytes);
    }
    else if (memcmp(commandDataBuffer, requestToUpdate, commandBufferMaxSize) == 0)
    {
        writingToFlash = true;
        transmitString("CTU");
    }
}