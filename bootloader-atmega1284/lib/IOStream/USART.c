#include <avr/io.h>
#include "USART.h"

/**
 * @brief Enables usart communication 
 * 
 * @note - 8 data bits
 * @note - 1 stop bit
 * @note - No parity
 * @note - Asynchronous operation
 * 
 * @attention A clock frequency of 18.432MHz and baud rate of 19.2kbs were used during the development of this bootloader.
 * Update F_CPU and the value in the UBRRn register to get the desired baud rate.
 * 
 */
void enableUSART(void)
{
    UBRR0L = 59;
    UCSR0B = _BV(RXEN0) | _BV(TXEN0);
    UCSR0C = _BV(UCSZ01) | _BV(UCSZ00);
}

/**
 * @brief Transmit data to the server.
 * 
 * @param data The data that will be transmitted.
 * @param length The length of the data array.
 * 
 */
void usartTransmit(const uint8_t data[], uint8_t length)
{
    for (uint8_t i = 0; i < length; ++i)
    {
        // Wait until the Transmitter is ready
        loop_until_bit_is_set(UCSR0A, UDRE0);
        UDR0 = data[i];
    }
}

/**
 * @brief Get the data from the usart receive data buffer and store it into the buffer.
 * 
 * @return Returns a data element struct that holds info about the data that was received.
 * 
 */
struct DataElement usartReceive(void)
{
    struct DataElement dataStruct;
    dataStruct.dataReceived = false;
    
    // Check the receive buffer for unread data.
    if (UCSR0A & _BV(RXC0))
    {
        dataStruct.dataReceived = true;
        dataStruct.data = UDR0;
    }

    return dataStruct;
}
