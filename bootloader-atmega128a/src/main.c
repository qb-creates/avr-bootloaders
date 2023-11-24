#include "BootloadUtility.h"
#include "CommandUtility.h"
#include "Timer.h"
#include "USART.h"
#include <avr/boot.h>
#include <avr/eeprom.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <stdbool.h>
#include <string.h>

uint8_t dataBuffer[259];

int main(void)
{
    enableTimer();
    enableUSART();

    bool applicationExist = pgm_read_word(0) != 0xFFFF;
    uint8_t resetTimer = 0;

    uint8_t bootloaderByte = eeprom_read_byte((uint8_t *)46);

    // Return to application section
    if (!pressAndHold(5) && bootloaderByte == 'c' && applicationExist)
    {
        disableTimer();
        disableUSART();
        asm("jmp 0x000");
    }

    // Continue to bootloader section.
    startBootloaderIndicator();

    while (true)
    {
        bool dataReceived = usartReceive(dataBuffer);
        checkForValidCommand(dataBuffer);
        bool uploadComplete = checkForPage(dataBuffer);

        if (dataReceived)
            resetTimer = 0;

        if (ETIFR & _BV(OCF3A) && bootloaderByte == 'c' && applicationExist)
        {
            ETIFR |= _BV(OCF3A);
            ++resetTimer;
        }

        // Exit bootloader.
        if (resetTimer >= 20)
            break;

        if (uploadComplete)
        {
            eeprom_update_byte((uint8_t *)46, 'c');
            break;
        }
    }

    // Reset mcu and registers.
    disableTimer();
    disableUSART();
    stopBootloaderIndicator();
    wdt_enable(WDTO_2S);
}