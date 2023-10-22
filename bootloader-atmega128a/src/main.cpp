#include "Timer.h"
#include "USART.h"
#include <avr/boot.h>
#include <avr/interrupt.h>
#include <avr/io.h>
#include <avr/pgmspace.h>
#include <avr/wdt.h>
#include <stdbool.h>
#include <util/delay.h>
// #include <avr/signature.h>

void resetMCU(void);
char writePageToFlash(volatile uint8_t *buf);
int isCommandValid(volatile uint8_t data[]);

const char *updateCommand = "RTU\r";

volatile uint8_t usartData[259];
volatile uint16_t dataCounter = 0;

volatile bool waitingForCommand = true;
volatile bool commandReceived = false;
volatile bool verifyFlash = false;
volatile bool writingToFlash = false;

ISR(USART1_RX_vect)
{
    usartData[dataCounter] = USART1::receiveData();

    if (waitingForCommand)
    {
        if (usartData[dataCounter] == '\r')
        {
            commandReceived = true;
            dataCounter = 0;
            waitingForCommand = false;
            return;
        }
    }

    if (writingToFlash)
    {
        USART1::transmitData("\r");
    }

    dataCounter++;
}

int main(void)
{
    DDRE = 0x00;
    PORTE = _BV(PE0);
    int time = 0;
    bool applicationExist = pgm_read_word(0) != 0xFFFF;
    // int watchdogResetFlag = MCUCSR & _BV(WDRF);
    // int powerOnResetFlag = MCUCSR & _BV(PORF);

    while (time < 7)
    {
        if (!(PINE & 0x01))
        {
            time++;
            _delay_ms(1000);
        }
        else
        {
            if (applicationExist)
            {
                // MCUCSR &= ~(watchdogResetFlag | powerOnResetFlag);
                cli();
                asm("jmp 0x000");
            }

            break;
        }
    }
    // bool applicationExist = pgm_read_word(0) != 0xFFFF;
    // int watchdogResetFlag = MCUCSR & _BV(WDRF);
    // int powerOnResetFlag = MCUCSR & _BV(PORF);

    // if ((watchdogResetFlag || powerOnResetFlag) && applicationExist)
    // {
    //     MCUCSR &= ~(watchdogResetFlag | powerOnResetFlag);
    //     cli();
    //     asm("jmp 0x000");
    // }

    // Move interrupt vectors to the begging of the bootloader section.
    MCUCR = _BV(IVCE);
    MCUCR = _BV(IVSEL);

    USART1::configure(3, Databits::Eight, Parity::None, StopBits::One, ClkPolarity::FallingRising, Mode::Async);
    TIMER::startResetTimer();
    TIMER::startBootloaderIndicator();
    sei();

    while (1)
    {
        if (commandReceived)
        {
            switch (isCommandValid(usartData))
            {
            case 0:
                USART1::transmitData("NACK");
                waitingForCommand = true;
                break;
            case 1:
                writingToFlash = true;
                USART1::transmitData("CTU");
                break;
            }

            commandReceived = false;
        }

        if (dataCounter == 259 && writingToFlash)
        {
            char finalByte = writePageToFlash(usartData);
            _delay_ms(50);

            if (finalByte != '\xFE')
            {
                dataCounter = 0;
                USART1::transmitData("Page");
            }
            else
            {
                USART1::transmitData("Complete");
                resetMCU();
            }
        }

        if (!writingToFlash && TIMER::resetTimer >= 20 && applicationExist)
        {
            resetMCU();
        }
    }
}

void resetMCU(void)
{
    TIMER::stopResetTimer();
    TIMER::stopBootloaderIndicator();
    wdt_enable(WDTO_15MS);
}

int isCommandValid(volatile uint8_t command[])
{
    for (int i = 0; i < 4; i++)
    {
        if (command[i] != updateCommand[i])
        {
            return 0;
        }
    }

    return 1;
}

char writePageToFlash(volatile uint8_t *buf)
{
    // Extract the page from the first two entries of the data array. Separates them into high and low bit.
    uint16_t pageNumberH = (*buf++) << 8;
    uint16_t pageNumberL = *buf++;

    // Get the page address by multiplying the page size times the page number.
    uint16_t pageAddress = (pageNumberH + pageNumberL) * SPM_PAGESIZE;

    cli();

    // Wait for any active eeprom writes to complete.
    eeprom_busy_wait();

    // Erase memory page.
    boot_page_erase(pageAddress);

    // Wait until the memory is erased.
    boot_spm_busy_wait();

    for (uint16_t i = 0; i < SPM_PAGESIZE; i += 2)
    {
        // Build opcode while setting up little-endian word.
        uint16_t opcode = *buf++;
        opcode += (*buf++) << 8;

        // Load page temp buffers
        boot_page_fill(pageAddress + i, opcode);
    }

    // Store buffer in flash page.
    boot_page_write(pageAddress);

    // Wait until the memory is written.
    boot_spm_busy_wait();

    // Enable Read While Write Section
    boot_rww_enable();
    sei();

    // Return the final bayte of the dataArray;
    return *buf;
}
