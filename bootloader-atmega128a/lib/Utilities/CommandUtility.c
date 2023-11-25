#include <avr/boot.h>
#include <string.h>
#include <avr/eeprom.h>
#include "CommandUtility.h"

const uint8_t commandBufferMaxSize = 4;
const char *requestSignatureBytes = "RSB";
const char *requestBootConfig = "RBC";
const char *requestToUpdate = "RTU";
const uint8_t signatureBytes[] = {0x1E, 0x97, 0x02};
const uint8_t clearToUpdateAck[] = {'C', 'T', 'U'};
const uint8_t pageAck[] = {'P', 'a', 'g', 'e'};
const uint8_t ack[] = {'\r'};

void checkForValidCommand(uint8_t *dataBuffer)
{
    if (!commandReceived)
        return;

    commandReceived = false;

    if (memcmp(dataBuffer, requestBootConfig, commandBufferMaxSize) == 0)
    {
        uint8_t highFuseBits[] = {boot_lock_fuse_bits_get(GET_HIGH_FUSE_BITS)};
        usartTransmit(highFuseBits, 1);
    }
    else if (memcmp(dataBuffer, requestSignatureBytes, commandBufferMaxSize) == 0)
    {
        usartTransmit(signatureBytes, 3);
    }
    else if (memcmp(dataBuffer, requestToUpdate, commandBufferMaxSize) == 0)
    {
        writeToFlash = true;
        OCR1A = (F_CPU / (2 * 1024 * 2)) - 1;
        eeprom_update_byte((uint8_t *)46, 'w');
        usartTransmit(clearToUpdateAck, 3);
    }
}

/**
 * @brief
 *
 * @param dataBuffer
 * @return Return true if all of the data has been recieved and written to the pages.
 * @return Return false if we are still waiting on more page data.
 */
bool checkForPage(uint8_t *dataBuffer)
{
    if (!pageReceived)
        return false;

    pageReceived = false;
    uint8_t finalByte = writeProgramDataToFlash(dataBuffer);

    // Acknowledge that the 259 bytes of data were received
    for (int i = 0; i < 259; ++i)
    {
        usartTransmit(ack, 1);
    }

    usartTransmit(pageAck, 4);

    return (finalByte == 0xFE);
}