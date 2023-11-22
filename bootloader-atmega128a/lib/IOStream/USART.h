#ifndef USART_H
#define USART_H

#include <stdint.h>
#include <stdbool.h>

extern const uint16_t programBufferMaxSize;
extern const uint8_t commandBufferMaxSize;

extern uint8_t programDataBuffer[259];
extern uint8_t commandDataBuffer[4];
extern uint16_t bufferCounter;
extern uint8_t commandBufferCounter;
extern bool commandReceived;
extern bool writingToFlash;
extern bool pageReceived;

void enableUSART();
void disableUSART();
void usartTransmit(const uint8_t data[], uint8_t length);
void usartReceive();

#endif