#include <avr/boot.h>
#include <stdbool.h>
#include "Bootloader.h"
/**
 * @brief Configure 16-bit Timer1. It is set to Compare Output Mode.
 * Output OC1A is toggled on compare match every 100ms (Focn = 5Hz). PB5 can be
 * connected to an LED to let the user know when the device is in
 * bootloader mode.720
 */
void startBootloaderIndicator()
{
    DDRB |= _BV(PB5);
    TCCR1B = _BV(WGM12) | _BV(CS12) | _BV(CS10);
    TCCR1A = _BV(COM1A0);
    OCR1A = (F_CPU / (2 * 1024 * 5)) - 1;
}

/**
 * @brief Stops the bootloader indicator output. Resets all Timer 1 registers back to 0x00;
 */
void stopBootloaderIndicator()
{
    TCCR1B = 0x00;
    TCCR1A = 0x00;
    DDRB = 0x00;
    PORTB = 0x00;
}

/**
 * @brief
 *
 * @return true
 * @return false
 */
uint8_t pressAndHold(uint8_t holdTime)
{
    DDRE = 0x00;
    PORTE = _BV(PE0);
    uint8_t timer = 0;

    while (!(PINE & 0x01))
    {
        if (ETIFR & _BV(OCF3A))
        {
            ETIFR |= _BV(OCF3A);
            ++timer;
        }

        if (timer == holdTime)
        {
            return true;
        }
    }

    return false;
}

/**
 * @brief Writes the page data
 *
 * @param buf
 * @return Returns the final byte that lets us know if al lthe data has been sent or if more page data is going to be sent.
 */
uint8_t writeProgramDataToFlash(uint8_t *buf)
{
    // Extract the page from the first two entries of the data array. Separates them into high and low bit.
    uint16_t pageNumberH = (*buf++) << 8;
    uint16_t pageNumberL = *buf++;

    // Get the page address by multiplying the page size times the page number.
    uint16_t pageAddress = (pageNumberH + pageNumberL) * SPM_PAGESIZE;

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

    // Return the final bayte of the dataArray;
    return *buf;
}
