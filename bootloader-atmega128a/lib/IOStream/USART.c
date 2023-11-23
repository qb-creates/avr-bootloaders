#include <avr/io.h>
#include "USART.h"

const uint16_t programBufferMaxSize = 259;
const uint8_t commandBufferMaxSize = 4;
const uint8_t ack[] = {'\r'};

// uint8_t programDataBuffer[259];
// uint8_t commandDataBuffer[4];
uint16_t bufferCounter = 0;
uint8_t commandBufferCounter = 0;
bool commandReceived = false;
bool writingToFlash = false;
bool pageReceived = false;

/**
 * @brief
 *
 */
void enableUSART()
{
    // Put the upper part of the baud number here (bits 8 to 11)
    UBRR1H = 0;
    UBRR1L = 3;

    // Configures the Registers
    UCSR1B = (1 << RXEN1) | (1 << TXEN1);
    UCSR1C = _BV(UCSZ11) | _BV(UCSZ10);
}

void disableUSART()
{
    UBRR1H = 0;
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
    for (uint8_t i = 0; i < length; i++)
    {
        // Wait until the Transmitter is ready
        loop_until_bit_is_set(UCSR1A, UDRE1);

        // Sends out data
        UDR1 = data[i];
    }
}

void usartReceive(uint8_t *buffer)
{
    if (UCSR1A & _BV(RXC1))
    {
        uint8_t data = UDR1;

        if (!writingToFlash)
        {
            buffer[commandBufferCounter] = data;
            commandBufferCounter++;

            if (data == '\0' || commandBufferCounter >= commandBufferMaxSize)
            {
                commandReceived = true;
                commandBufferCounter = 0;
            }

            return;
        }

        buffer[bufferCounter] = data;
        bufferCounter++;

        usartTransmit(ack, 1);

        if (bufferCounter == programBufferMaxSize)
        {
            bufferCounter = 0;
            pageReceived = true;
        }
    }
}
