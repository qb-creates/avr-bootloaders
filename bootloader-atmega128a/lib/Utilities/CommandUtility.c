#include <avr/boot.h>
#include <avr/eeprom.h>
#include <avr/wdt.h>
#include <string.h>
#include "CommandUtility.h"

// Bootloader Status
uint8_t *bootloaderStatusAddress = (uint8_t *)46;
const uint8_t uploadCompleteCode = 'c';
const uint8_t uploadeFailedCode = 'w';

// Accepted Commands
const char *requestToUpdate = "RTU";

// Command Responses
const uint8_t pageAck[] = {'P', 'a', 'g', 'e'};
const uint8_t ack[] = {'\r'};

const uint8_t commandBufferMaxSize = 4;
const uint8_t uploadCompleteByte = 0xFE;

/**
 * @brief Finds the first command in the buffer and executes it
 *
 * @param dataBuffer Data buffer that holds the command that was just sent to the microcontroller
 */
void executeCommand(uint8_t *dataBuffer)
{
    if (!memcmp(dataBuffer, requestToUpdate, commandBufferMaxSize))
    {
        uint8_t ack[] = {0x1E, 0x97, 0x02, boot_lock_fuse_bits_get(GET_HIGH_FUSE_BITS), 'C', 'T', 'U'};
        eeprom_update_byte(bootloaderStatusAddress, uploadeFailedCode);
        setBootloaderIndicatorFrequency(2);
        usartTransmit(ack, 7);
    }
}

/**
 * @brief
 *
 * @param dataBuffer
 * @return Return true if all of the data has been recieved and written to the pages.
 * @return Return false if we are still waiting on more page data.
 */
void checkForPage(uint8_t *dataBuffer)
{
    uint8_t finalByte = writeProgramDataToFlash(dataBuffer);

    // Acknowledge that the 259 bytes of data were received
    for (int i = 0; i < 259; ++i)
    {
        usartTransmit(ack, 1);
    }

    usartTransmit(pageAck, 4);

    if (finalByte == uploadCompleteByte)
    {
        eeprom_update_byte(bootloaderStatusAddress, uploadCompleteCode);
        stopBootloaderIndicator();
        wdt_enable(WDTO_2S);
        while (1)
            ;
    }
}