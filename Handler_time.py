import datetime
import time
import pytz

class TimeHandlerSingleton:
    _instance = None
    
    @classmethod
    def get_instance(cls):
        """Get the singleton instance of TimeHandler."""
        if cls._instance is None:
            cls._instance = TimeHandler()
        return cls._instance


class TimeHandler:
    def __init__(self):
        """Initialize the TimeHandler."""
        pass
        
    def get_current_datetime(self, timezone=None):
        """Get the current date and time, optionally in the specified timezone.
        
        Args:
            timezone (str, optional): Timezone name (e.g., 'Europe/London'). Defaults to local time.
            
        Returns:
            datetime.datetime: Current datetime object
        """
        if timezone:
            tz = pytz.timezone(timezone)
            return datetime.datetime.now(tz)
        return datetime.datetime.now()
    
    def get_current_date(self, format_str="%Y-%m-%d", timezone=None):
        """Get the current date as a formatted string.
        
        Args:
            format_str (str, optional): Date format string. Defaults to "%Y-%m-%d".
            timezone (str, optional): Timezone name. Defaults to local time.
            
        Returns:
            str: Formatted date string
        """
        current_dt = self.get_current_datetime(timezone)
        return current_dt.strftime(format_str)
    
    def get_current_time(self, format_str="%H:%M:%S", timezone=None):
        """Get the current time as a formatted string.
        
        Args:
            format_str (str, optional): Time format string. Defaults to "%H:%M:%S".
            timezone (str, optional): Timezone name. Defaults to local time.
            
        Returns:
            str: Formatted time string
        """
        current_dt = self.get_current_datetime(timezone)
        return current_dt.strftime(format_str)
    
    def get_datetime_for_docx(self, timezone=None):
        """Get a formatted datetime string suitable for DOCX documents.
        
        Args:
            timezone (str, optional): Timezone name. Defaults to local time.
            
        Returns:
            str: Formatted datetime string (e.g., "April 2025")
        """
        current_dt = self.get_current_datetime(timezone)
        return current_dt.strftime("%B %Y")
    
    def get_timestamp(self, format_str="%Y-%m-%d %H:%M:%S", timezone=None):
        """Get a timestamp in the specified format.
        
        Args:
            format_str (str, optional): Timestamp format. Defaults to "%Y-%m-%d %H:%M:%S".
            timezone (str, optional): Timezone name. Defaults to local time.
            
        Returns:
            str: Formatted timestamp
        """
        current_dt = self.get_current_datetime(timezone)
        return current_dt.strftime(format_str)
    
    def get_timestamp_for_log(self, timezone=None):
        """Get a timestamp formatted for log entries.
        
        Args:
            timezone (str, optional): Timezone name. Defaults to local time.
            
        Returns:
            str: Timestamp in format "[YYYY-MM-DD HH:MM]"
        """
        current_dt = self.get_current_datetime(timezone)
        return current_dt.strftime("[%Y-%m-%d %H:%M]")
    
    def get_date_difference(self, start_date, end_date, unit='days'):
        """Calculate the difference between two dates.
        
        Args:
            start_date (datetime.datetime): Start date
            end_date (datetime.datetime): End date
            unit (str, optional): Unit of difference ('days', 'hours', 'minutes', 'seconds'). 
                                  Defaults to 'days'.
            
        Returns:
            float: Difference in the specified unit
        """
        if isinstance(start_date, str):
            start_date = datetime.datetime.strptime(start_date, "%Y-%m-%d")
        if isinstance(end_date, str):
            end_date = datetime.datetime.strptime(end_date, "%Y-%m-%d")
            
        diff = end_date - start_date
        
        if unit == 'days':
            return diff.days
        elif unit == 'hours':
            return diff.total_seconds() / 3600
        elif unit == 'minutes':
            return diff.total_seconds() / 60
        elif unit == 'seconds':
            return diff.total_seconds()
        else:
            return diff.days
    
    def parse_date_string(self, date_str, format_str="%Y-%m-%d"):
        """Parse a date string into a datetime object.
        
        Args:
            date_str (str): Date string to parse
            format_str (str, optional): Format of the date string. Defaults to "%Y-%m-%d".
            
        Returns:
            datetime.datetime: Parsed datetime object
        """
        return datetime.datetime.strptime(date_str, format_str)
    
    def format_date(self, dt, format_str="%Y-%m-%d"):
        """Format a datetime object into a string.
        
        Args:
            dt (datetime.datetime): Datetime object to format
            format_str (str, optional): Output format. Defaults to "%Y-%m-%d".
            
        Returns:
            str: Formatted date string
        """
        if isinstance(dt, str):
            dt = self.parse_date_string(dt)
        return dt.strftime(format_str)


# Create a convenient function to get the singleton instance
def get_time_handler():
    """Get the singleton instance of TimeHandler."""
    return TimeHandlerSingleton.get_instance()


# Example usage if run as a script
if __name__ == "__main__":
    handler = get_time_handler()
    
    print(f"Current datetime: {handler.get_current_datetime()}")
    print(f"Current date: {handler.get_current_date()}")
    print(f"Current time: {handler.get_current_time()}")
    print(f"DOCX datetime: {handler.get_datetime_for_docx()}")
    print(f"Timestamp: {handler.get_timestamp()}")
    print(f"Log timestamp: {handler.get_timestamp_for_log()}")
    
    # Date difference example
    today = handler.get_current_datetime()
    future = today + datetime.timedelta(days=30)
    print(f"Days until future date: {handler.get_date_difference(today, future)}") 