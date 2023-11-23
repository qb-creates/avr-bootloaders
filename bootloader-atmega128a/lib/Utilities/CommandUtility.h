#ifndef COMMANDS_H
#define COMMANDS_H
#include <stdint.h>
#include <stdbool.h>

const uint8_t commandBufferMaxSize;

void checkForValidCommand(uint8_t *dataBuffer);
bool checkForPage(uint8_t *dataBuffer);

#endif