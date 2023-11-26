#include <avr/io.h>
#include "USART.h"

const uint16_t bufferMaxSize = 259;
uint16_t bufferCounter = 0;
bool writeToFlash = false;

/**
 * @brief Enables usart communication 
 * 
 * @note 8 data bits
 * @note 1 stop bit
 * @note No parity
 * @note Asynchronous operation
 * @note 115.2kbps baud rate 
 */
void enableUSART()
{
    // Using a clock frequency of 7372800 Hz, set UBRR1 to 3 for a baud rate of 112.5kbps.
    UBRR1L = 3;

    // Enable transmitter and receiver
    UCSR1B = _BV(RXEN1) | _BV(TXEN1);

    // Asynchrounous operation, 8 data bits, 1 stop bit, no parity, asynchronous
    UCSR1C = _BV(UCSZ11) | _BV(UCSZ10);
}

/**
 * @brief Transmit data.
 * 
 * @param data The data that will be transmitted.
 * @param length The length of the data array.
 */
void usartTransmit(const uint8_t data[], uint8_t length)
{
    for (uint8_t i = 0; i < length; ++i)
    {
        // Wait until the Transmitter is ready
        loop_until_bit_is_set(UCSR1A, UDRE1);
        UDR1 = data[i];
    }
}

/**
 * @brief Get the data from the usart receive data buffer and store it into the buffer 
 * 
 * @param buffer Buffer where the received data will be stored.
 * @return Returns the
 */
enum DataString usartReceive(uint8_t *buffer)
{
    // If there is unread data in the receive buffer 
    if (UCSR1A & _BV(RXC1))
    {
        uint8_t data = UDR1;

        buffer[bufferCounter] = data;
        ++bufferCounter;

        if (data == '\0' && !writeToFlash)
        {
            bufferCounter = 0;
            return CommandString;
        }

        if (bufferCounter == bufferMaxSize)
        {
            bufferCounter = 0;
            return PageString;
        }
    }

    return IncompleteString;
}
