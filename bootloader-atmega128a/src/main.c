#include "Bootloader.h"
#include "Timer.h"
#include "USART.h"
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <string.h>
#include <util/delay.h>

void executeCommand(void);

const char *getBootConfigCommand = "BOOTCONFIG\0";
const char *updateCommand = "RTU\0";

int main(void)
{
    bool applicationExist = pgm_read_word(0) != 0xFFFF;
    int resetTimer = 0;

    enableTimer();

    if (!pressAndHold(5) && applicationExist)
    {
        disableTimer();
        asm("jmp 0x000");
    }

    // Move interrupt vectors to the begging of the bootloader section.
    MCUCR = _BV(IVCE);
    MCUCR = _BV(IVSEL);

    configureUSART();
    startBootloaderIndicator();
    sei();

    while (1)
    {
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
    transmitString("Complete");
    wdt_enable(WDTO_15MS);
}

void executeCommand(void)
{
    char commandData[commandBufferMaxSize];

    for (int i = 0; i < commandBufferMaxSize; i++)
    {
        commandData[i] = commandDataBuffer[i];
    }

    if (strcmp(commandData, getBootConfigCommand) == 0)
    {
        unsigned char highFuseBits = boot_lock_fuse_bits_get(GET_HIGH_FUSE_BITS);
        transmitChar(highFuseBits);
    }
    else if (strcmp(commandData, updateCommand) == 0)
    {
        writingToFlash = true;
        transmitString("CTU");
    }
}