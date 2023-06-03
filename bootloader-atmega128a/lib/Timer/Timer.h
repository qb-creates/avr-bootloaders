#ifndef TIMER_H
#define TIMER_H
#include <avr/io.h>
#include <avr/interrupt.h>

class TIMER
{
public:
	static volatile int resetTimer;
	static void startResetTimer();
	static void stopResetTimer();
	static void startBootloaderIndicator();
	static void stopBootloaderIndicator();
};

#endif