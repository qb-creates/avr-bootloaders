#ifndef USART_H
#define USART_H
#include <avr/io.h>
#include <stdio.h>
#include <string.h>
#include <stdbool.h>

extern const uint16_t programBufferMaxSize;
extern const uint8_t commandBufferMaxSize;

extern volatile unsigned char programDataBuffer[259];
extern volatile unsigned char commandDataBuffer[11];
extern volatile uint16_t bufferCounter;
extern volatile uint8_t commandBufferCounter;
extern volatile bool commandReceived;
extern volatile bool writingToFlash;
extern volatile bool pageReceived;

void configureUSART();
void transmitString(const char *data);
void transmitChar(unsigned char data);

#endif