#ifndef USART_H
#define USART_H

#include <stdbool.h>
#include <stdint.h>

/**
 * @brief sdfsdf
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