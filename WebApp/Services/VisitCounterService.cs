namespace WebApp.Services
{
    public class VisitCounterService
    {
        private int visitCounter = 0;

        public int GetVisitCount()
        {
            return this.visitCounter;
        }

        public void incrementVisitCounter()
        {
            this.visitCounter++;
        }

    }
}
