using System;

namespace Course {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Qual hora atual?");
            int hora = int.Parse (Console.ReadLine());

            if (hora < 12) {
                Console.WriteLine("Bom dia!");
            }

            else if (hora < 18) {
                Console.WriteLine("Boa tarde!");
            }
            
            else if (hora < 25) {
                Console.WriteLine("Boa tarde!");
            }

            else {
                Console.WriteLine("Hora Invalida!");
            }
        }
    }
}

