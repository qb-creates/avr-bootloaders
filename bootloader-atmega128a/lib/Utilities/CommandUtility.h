#ifndef COMMANDS_H
#define COMMANDS_H
#include <stdint.h>
#include <stdbool.h>

void checkForValidCommand(uint8_t *dataBuffer);
bool checkForPage(uint8_t *dataBuffer);

#endif