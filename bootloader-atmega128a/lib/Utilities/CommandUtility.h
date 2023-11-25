#ifndef COMMANDS_H
#define COMMANDS_H

#include "BootloadUtility.h"
#include "USART.h"

const uint8_t commandBufferMaxSize;

void checkForValidCommand(uint8_t *dataBuffer);
bool checkForPage(uint8_t *dataBuffer);

#endif