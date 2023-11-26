#include <avr/boot.h>
#include "BootloadUtility.h"

/**
 * @brief Configure 16-bit Timer1. It is set to Compare Output Mode.
 * Output OC1A is toggled on compare match every 100ms (Focn = 5Hz). PB5 can be
 * connected to an LED to let the user know when the device is in
 * bootloader mode.720
 */
void startBootloaderIndicator(uint8_t frequency)
{
    DDRB |= _BV(PB5);
    TCCR1B = _BV(WGM12) | _BV(CS12) | _BV(CS10);
    TCCR1A = _BV(COM1A0);
    setBootloaderIndicatorFrequency(frequency);
}

/**
 * @brief Stops the bootloader indicator output. Resets all Timer 1 registers back to 0x00;
 */
void stopBootloaderIndicator(void)
{
    TCCR1B = 0x00;
    TCCR1A = 0x00;
    DDRB = 0x00;
    PORTB = 0x00;
}

/**
 * @brief Set the Bootloader Indicator Frequency object
 * 
 * @param frequency 
 */
void setBootloaderIndicatorFrequency(uint8_t frequency)
{
    OCR1A = (F_CPU / (2 * 1024 * frequency)) - 1;
}

/**
 * @brief Writes the page data and returns the page status byte.
 *
 * @param buf Buffer that holds the page data. The buffer should be 259 bytes.
 * @return Returns the page status byte. 0xFE indicates that the last page of data has been sent
 * @note Page buffer should be 259 bytes. 
 * @note Bytes 1 and 2 is the page address.
 * @note Bytes 3 through 258 is the 256 bytes of program data that will be written.
 * @note Byte 259 is the page status byte.
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
        // Get program data word and set up as little-endia.
        uint16_t w = *buf++;
        w += (*buf++) << 8;

        // Load page temp buffers
        boot_page_fill(pageAddress + i, w);
    }

    // Store buffer in flash page.
    boot_page_write(pageAddress);

    // Wait until the memory is written.
    boot_spm_busy_wait();

    // Enable Read While Write Section
    boot_rww_enable();

    // Return the page status byte;
    return *buf;
}
