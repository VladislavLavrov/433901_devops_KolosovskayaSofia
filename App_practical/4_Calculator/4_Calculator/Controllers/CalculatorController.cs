using Microsoft.AspNetCore.Mvc;
namespace Calculator.Controllers
{
    public enum Operation
    {
        Add, Subtract, Multiply, Divide
    }
    public class CalculatorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calculate(double num1, double
       num2, Operation operation)
        {
            double result = 0;
            switch (operation)
            {
                case Operation.Add:
                    result = num1 + num2;
                    break;
                case Operation.Subtract:
                    result = num1 - num2;
                    break;
                case Operation.Multiply:
                    result = num1 * num2;
                    break;
                case Operation.Divide:
                    result = num1 / num2;
                    break;
            }
            ViewBag.Result = result;
            return View("Index");
        }
    }
}