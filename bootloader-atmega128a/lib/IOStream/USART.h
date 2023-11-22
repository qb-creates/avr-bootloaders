#ifndef USART_H
#define USART_H
#include <avr/io.h>
#include <stdio.h>
#include <string.h>
#include <stdbool.h>

extern const uint16_t programBufferMaxSize;
extern const uint8_t commandBufferMaxSize;

extern unsigned char programDataBuffer[259];
extern unsigned char commandDataBuffer[4];
extern uint16_t bufferCounter;
extern uint8_t commandBufferCounter;
extern bool commandReceived;
extern bool writingToFlash;
extern bool pageReceived;

void configureUSART();
void transmitString(const char *data);
void transmitChar(unsigned char data);
void receiveData();

#endif