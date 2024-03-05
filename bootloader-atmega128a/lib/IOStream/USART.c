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
 * @attention A clock frequency of 7.3728MHz and baud rate of 19.2kbs were used during the development of this bootloader.
 * Update F_CPU and the value in the UBRRn register to get the desired baud rate.
 * 
 */
void enableUSART(void)
{
    UBRR1L = 23;
    UCSR1B = _BV(RXEN1) | _BV(TXEN1);
    UCSR1C = _BV(UCSZ11) | _BV(UCSZ10);
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
        loop_until_bit_is_set(UCSR1A, UDRE1);
        UDR1 = data[i];
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
    if (UCSR1A & _BV(RXC1))
    {
        dataStruct.dataReceived = true;
        dataStruct.data = UDR1;
    }

    return dataStruct;
}
