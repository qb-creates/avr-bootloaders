#ifndef USART_H
#define USART_H

#include <stdbool.h>
#include <stdint.h>

/**
 * @brief USART data element.
 * 
 * @note - Indicates if data was successfully read from the usart receive buffers.
 * @note - Holds the data read from the usart receive buffers.
 * 
 */
struct DataElement
{
   bool dataReceived;
   uint8_t data;
};

void enableUSART(void);
void usartTransmit(const uint8_t data[], uint8_t length);
struct DataElement usartReceive(void);

#endif