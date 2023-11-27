#ifndef USART_H
#define USART_H

#include <stdbool.h>
#include <stdint.h>

/**
 * @brief sdfsdf
 * 
 */
struct DataStruct
{
   bool dataReceived;
   uint8_t data;
};

void enableUSART();
void usartTransmit(const uint8_t data[], uint8_t length);
struct DataStruct usartReceive();

#endif