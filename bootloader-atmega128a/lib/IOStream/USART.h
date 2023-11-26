#ifndef USART_H
#define USART_H

#include <stdint.h>
#include <stdbool.h>

extern const uint16_t bufferMaxSize;

extern uint16_t bufferCounter;
extern bool writeToFlash;
extern bool commandReceived;
extern bool pageReceived;

void enableUSART();
void usartTransmit(const uint8_t data[], uint8_t length);
bool usartReceive(uint8_t *buffer);

#endif