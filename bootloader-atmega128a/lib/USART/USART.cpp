#include <USART.h>

/**
 * Enables the USART transmitter and receiver.
 * @param[in] ubrr baud rate.
 * @param[in] dataBits Number of databits.
 * @param[in] parity None, Even, or Odd.
 * @param[in] stopBits Number of stop bits.
 * @param[in] clock Clock polarity.
 * @param[in] mode Asynchronous or Synchronous mode.
 * */
void USART1::configure(int ubrr, Databits dataBits, Parity parity, StopBits stopBits, ClkPolarity clock, Mode mode)
{
    // Separates the three UCSZ bits in the databits enum. Shift the bit by the proper ammount to set it in the register.
    int ucsz12 = ((static_cast<int>(dataBits) & 0x04) == 0x04) << UCSZ12;
    int ucsz11 = ((static_cast<int>(dataBits) & 0x02) == 0x02) << UCSZ11;
    int ucsz10 = (static_cast<int>(dataBits) & 0x01) << UCSZ10;

    // Separates the two UMP bits in the Parity enum. Shift the bit by the proper ammount to set it in the register.
    int ump11 = ((static_cast<int>(parity) & 0x02) == 0x02) << UPM11;
    int ump10 = (static_cast<int>(parity) & 0x01) << UPM10;

    // Shift the bit by the proper ammount to set it in the register.
    int usbs1 = static_cast<int>(stopBits) << USBS1;
    int umsel1 = static_cast<int>(mode) << UMSEL1;
    int ucpol1 = static_cast<int>(clock) << UCPOL1;

    // Put the upper part of the baud number here (bits 8 to 11)
    UBRR1H = (unsigned char)(ubrr >> 8);

    // Put the remaining part of the baud number here
    UBRR1L = (unsigned char)ubrr;

    // Configures the Registers
    UCSR1B = (1 << RXCIE1) | (1 << RXEN1) | (1 << TXEN1) | ucsz12;
    UCSR1C = umsel1 | ump11 | ump10 | usbs1 | ucsz11 | ucsz10 | ucpol1;
}

/**
 * Retrieve data from the UDR1 Register
 * @return Returns the data in the UDR1 register.
 * */
unsigned char USART1::receiveData(void)
{
    loop_until_bit_is_set(UCSR1A, RXC1);
    return UDR1;
}

/**
 * Transmit data.
 * @param[in] data Data that will be transmitted.
 * */
void USART1::transmitData(const char *data)
{
    size_t size = strlen(data);
    
    for (unsigned int i = 0; i < size; i++)
    {
        // Wait until the Transmitter is ready
        loop_until_bit_is_set(UCSR1A, UDRE1);

        // Sends out data
        UDR1 = data[i];
    }
}

/**
 * Transmit data.
 * @param[in] data Data that will be transmitted.
 * @param[in] size The size of the data that will be transmitted
 * */
void USART1::transmitData(char data[], int size)
{
    for (int i = 0; i < size; i++)
    {
        // Wait until the Transmitter is ready
        loop_until_bit_is_set(UCSR1A, UDRE1);

        // Sends out data
        UDR1 = data[i];
    }
}