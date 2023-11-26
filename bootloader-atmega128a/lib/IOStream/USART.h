#ifndef USART_H
#define USART_H

#include <stdbool.h>
#include <stdint.h>

/**
 * @brief sdfsdf
 * 
 */
enum DataString
{
    CommandString,
    PageString,
    IncompleteString
};

extern const uint16_t bufferMaxSize;
extern uint16_t bufferCounter;
extern bool writeToFlash;

void enableUSART();
void usartTransmit(const uint8_t data[], uint8_t length);
enum DataString usartReceive(uint8_t *buffer);

#endif