#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <string.h>
#include <stdbool.h>
#include "BootloadUtility.h"
#include "Timer.h"
#include "USART.h"
#include "CommandUtility.h"

uint8_t dataBuffer[259];

int main(void)
{
    enableTimer();
    enableUSART();

    bool applicationExist = pgm_read_word(0) != 0xFFFF;
    uint8_t resetTimer = 0;

    // Return to application section 
    if (!pressAndHold(5) && applicationExist)
    {
        disableTimer();
        disableUSART();
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    startBootloaderIndicator();

    while (true)
    {
        usartReceive(dataBuffer);

        if (ETIFR & _BV(OCF3A))
        {
            ETIFR |= _BV(OCF3A);
            resetTimer++;
        }
        
        checkForValidCommand(dataBuffer);
        bool uploadComplete = checkForPage(dataBuffer);

        // Exit bootloader.
        if ((!writingToFlash && resetTimer >= 20 && applicationExist) || uploadComplete)
            break;
    }

    // Reset mcu and registers.
    disableTimer();
    disableUSART();
    stopBootloaderIndicator();
    wdt_enable(WDTO_15MS);
}