#ifndef COMMANDS_H
#define COMMANDS_H

#include "BootloadUtility.h"
#include "USART.h"

const uint8_t commandBufferMaxSize;
const uint8_t uploadCompleteByte;
const uint8_t uploadCompleteCode;
const uint8_t uploadeFailedCode;
uint8_t *bootloaderStatusAddress;
void executeCommand(uint8_t *dataBuffer);
void checkForPage(uint8_t *dataBuffer);

#endif
