export const formatTime12Hour = (timeString: string | undefined | null) => {
  if (!timeString || timeString === '-') return '-';
  
  try {
    const parts = timeString.split(':');
    if (parts.length < 2) return timeString;
    
    let h = parseInt(parts[0], 10);
    const m = parts[1];
    const ampm = h >= 12 ? 'PM' : 'AM';
    
    h = h % 12;
    h = h ? h : 12; // 0 should be 12
    
    return `${h.toString().padStart(2, '0')}:${m} ${ampm}`;
  } catch {
    return timeString;
  }
};
