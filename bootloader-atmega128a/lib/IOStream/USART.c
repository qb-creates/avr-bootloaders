#include <avr/io.h>
#include "USART.h"

const uint16_t bufferMaxSize = 259;

uint16_t bufferCounter = 0;
bool writeToFlash = false;
bool commandReceived = false;
bool pageReceived = false;

/**
 * @brief
 *
 */
void enableUSART()
{
    // Put the upper part of the baud number here (bits 8 to 11)
    UBRR1L = 3;

    // Configures the Registers
    UCSR1B = _BV(RXEN1) | _BV(TXEN1);
    UCSR1C = _BV(UCSZ11) | _BV(UCSZ10);
}

void disableUSART()
{
    UBRR1L = 0;
    UCSR1B = 0;
    UCSR1C = 0;
}

/**
 * Transmit data.
 * @param[in] data Data that will be transmitted.
 * */
void usartTransmit(const uint8_t data[], uint8_t length)
{
    for (uint8_t i = 0; i < length; ++i)
    {
        // Wait until the Transmitter is ready
        loop_until_bit_is_set(UCSR1A, UDRE1);
    
        // Sends out data
        UDR1 = data[i];
    }
}

bool usartReceive(uint8_t *buffer)
{
    if (UCSR1A & _BV(RXC1))
    {
        uint8_t data = UDR1;

        buffer[bufferCounter] = data;
        ++bufferCounter;

        if (data == '\0' && !writeToFlash)
        {
            bufferCounter = 0;
            commandReceived = true;
            return true;
        }

        if (bufferCounter == bufferMaxSize)
        {
            bufferCounter = 0;
            pageReceived = true;
            return true;
        }
    }

    return false;
}
