#include <avr/io.h>
#include "USART.h"

/**
 * @brief Enables usart communication 
 * 
 * @note 8 data bits
 * @note 1 stop bit
 * @note No parity
 * @note Asynchronous operation
 * @note Using a clock frequency of 7372800 Hz, set UBRR1 to 3 for a baud rate of 115.2kbps
 */
void enableUSART(void)
{
    UBRR1L = 3;
    UCSR1B = _BV(RXEN1) | _BV(TXEN1);
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
 * @return Returns the
 */
struct DataElement usartReceive(void)
{
    struct DataElement dataStruct;
    dataStruct.dataReceived = false;
    
    // If there is unread data in the receive buffer 
    if (UCSR1A & _BV(RXC1))
    {
        dataStruct.dataReceived = true;
        dataStruct.data = UDR1;
    }

    return dataStruct;
}
