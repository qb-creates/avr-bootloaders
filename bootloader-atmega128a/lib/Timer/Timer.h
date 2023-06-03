#ifndef TIMER_H
#define TIMER_H
#include <avr/io.h>
#include <avr/interrupt.h>

class TIMER
{
public:
	static volatile int resetTimer;
	static void stopBootloaderTimer();
	static void startBootloaderIndicator();
	static void stopBootloaderIndicator();
};

class TIMER0
{
public:
	static void configure();
};
#endif