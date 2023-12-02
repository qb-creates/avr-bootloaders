#include <avr/boot.h>
#include "BootloadUtility.h"

// Bootloader Status
uint8_t *bootloaderStatusAddress = (uint8_t *)46;
const uint8_t uploadCompleteCode = 'c';
const uint8_t uploadeFailedCode = 'w';

// Command Responses
const uint8_t pageAck[] = {'P', 'a', 'g', 'e'};
const uint8_t ack[] = {'\r'};

const uint8_t uploadCompleteByte = 0xFE;

/**
 * @brief Starts the bootload process. The devices signature, high fuse bits, and 'CTU' is
 * sent back to the server.
 * 
 */
void startBootloadProcess(void)
{
    // Store the devices signature, high fuse bits, and "CTU" in an array.
    uint8_t ack[] = {0x1E, 0x97, 0x02, boot_lock_fuse_bits_get(GET_HIGH_FUSE_BITS), 'C', 'T', 'U'};

    OCR1A = (F_CPU / (2 * 1024 * 2)) - 1;
    eeprom_update_byte(bootloaderStatusAddress, uploadeFailedCode);
    usartTransmit(ack, 7);
}

/**
 * @brief Configure 16-bit Timer1.
 *
 * @note - Output compare mode.
 * @note - Indicator light should be connected to OC1A located on Pin B5.
 * @note - OC1A toggles on each output compare match.
 * @note - OCR1A = (F_CPU / (2 * 1024 * frequency)) - 1;
 * 
 * @attention A clock frequency of 7.3728MHz was used during the development of this bootloader.
 * Update F_CPU and the value of the 'frequency' variable to get the desired indicator flash frequency.
 * 
 */
void startBootloadIndicator(void)
{
    DDRB |= _BV(PB5);
    TCCR1B = _BV(WGM12) | _BV(CS12) | _BV(CS10);
    TCCR1A = _BV(COM1A0);
    OCR1A = (F_CPU / (2 * 1024 * 5)) - 1;
}

/**
 * @brief Writes the page data and returns the page status byte.
 *
 * @param buf Buffer that holds the page data. The buffer should be 259 bytes.
 * 
 * @note - Page buffer should be 259 bytes.
 * @note - Bytes 1 and 2 indicate the page address.
 * @note - Bytes 3 through 258 are the 256 bytes of program data that will be written.
 * @note - Byte 259 indicates the page status byte.
 * 
 */
void writeProgramDataToFlash(uint8_t *buf)
{
    // Extract the page from the first two entries of the data array. Separates them into high and low bit.
    uint16_t pageNumberH = (*buf++) << 8;
    uint16_t pageNumberL = *buf++;

    // Get the page address by multiplying the page size times the page number.
    uint16_t pageAddress = (pageNumberH + pageNumberL) * SPM_PAGESIZE;

    eeprom_busy_wait();
    boot_page_erase(pageAddress);
    boot_spm_busy_wait();

    for (uint16_t i = 0; i < SPM_PAGESIZE; i += 2)
    {
        // Get program data word and set up as little-endian.
        uint16_t word = *buf++;
        word += (*buf++) << 8;

        // Load page temp buffers
        boot_page_fill(pageAddress + i, word);
    }

    boot_page_write(pageAddress);
    boot_spm_busy_wait();
    boot_rww_enable();

    // Acknowledge that the 259 bytes of data were received
    for (int i = 0; i < 259; ++i)
    {
        usartTransmit(ack, 1);
    }

    usartTransmit(pageAck, 4);

    // Check page status byte.
    if (*buf == uploadCompleteByte)
    {
        eeprom_update_byte(bootloaderStatusAddress, uploadCompleteCode);
        wdt_enable(WDTO_1S);

        // Loop until the microcontroller resets.
        while (true);
    }
}
