#include "USART.h"
#include <avr/interrupt.h>

const uint16_t programBufferMaxSize = 259;
const uint8_t commandBufferMaxSize = 4;

unsigned char programDataBuffer[259];
unsigned char commandDataBuffer[4];
uint16_t bufferCounter = 0;
uint8_t commandBufferCounter = 0;
bool commandReceived = false;
bool writingToFlash = false;
bool pageReceived = false;

/**
 * @brief
 *
 */
void configureUSART()
{
    // Put the upper part of the baud number here (bits 8 to 11)
    UBRR1H = 0;
    UBRR1L = 3;

    // Configures the Registers
    UCSR1B = (1 << RXEN1) | (1 << TXEN1);
    UCSR1C = _BV(UCSZ11) | _BV(UCSZ10);
}

/**
 * Transmit data.
 * @param[in] data Data that will be transmitted.
 * */
void transmitString(const char *data)
{
    size_t size = strlen(data);

    for (unsigned int i = 0; i < size; i++)
    {
        // Wait until the Transmitter is ready
        loop_until_bit_is_set(UCSR1A, UDRE1);

        // Sends out data
        UDR1 = data[i];
    }
}

/**
 * @brief
 *
 * @param data
 */
void transmitChar(unsigned char data)
{
    // Wait until the Transmitter is ready
    loop_until_bit_is_set(UCSR1A, UDRE1);

    // Sends out data
    UDR1 = data;
}

void receiveData()
{
    if (UCSR1A & _BV(RXC1))
    {
        unsigned char data = UDR1;

        if (!writingToFlash)
        {
            commandDataBuffer[commandBufferCounter] = data;
            commandBufferCounter++;

            if (data == '\0' || commandBufferCounter >= commandBufferMaxSize)
            {
                commandReceived = true;
                commandBufferCounter = 0;
            }

            return;
        }

        programDataBuffer[bufferCounter] = data;
        bufferCounter++;
        transmitString("\r");

        if (bufferCounter == programBufferMaxSize)
        {
            bufferCounter = 0;
            pageReceived = true;
        }
    }
}
