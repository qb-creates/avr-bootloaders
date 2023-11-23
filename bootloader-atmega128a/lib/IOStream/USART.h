#ifndef USART_H
#define USART_H

#include <stdint.h>
#include <stdbool.h>

extern const uint16_t bufferMaxSize;

extern uint16_t bufferCounter;
extern bool listenForCommand;
extern bool commandReceived;
extern bool pageReceived;

void enableUSART();
void disableUSART();
void usartTransmit(const uint8_t data[], uint8_t length);
bool usartReceive(uint8_t *buffer);

#endif